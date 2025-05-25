using MyTelegramBot.Classes;
using MyTelegramBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MyTelegramBot.HandleUpdates
{
    internal class HandleUpdatesUser : CustomHandleUpdates
    {
        private TelegramSession BotSession;
        public HandleUpdatesUser(TelegramSession ABotSession, HandleUpdatesMain AHandleUpdatesUtils)
        {
            BotSession = ABotSession;
            _HandleUpdatesUtils = AHandleUpdatesUtils;
        }
        private ICustomQuery? UserQuery { get => BotSession?.QueryUser; }

        private HandleUpdatesMain _HandleUpdatesUtils;
        public HandleUpdatesMain HandleUpdatesUtils { get => _HandleUpdatesUtils; }

        private const string _userquerysbord = "userquerysbord";
        private List<CustomUserTopic>? TopicList = null;

        protected override async Task DoGetMenuRole(ITelegramBotClient AbotClient, Update? Aupdate, RolesEnum? ARole_Id)
        {
            if (ARole_Id == RolesEnum.reUser)
            {
                await ShowUserButtons(AbotClient, Aupdate, 0);
                if (_HandleUpdatesUtils != null)
                    await _HandleUpdatesUtils.ShowDefaultButtons(AbotClient, Aupdate, 0);
            }
        }

        public List<CustomUserTopic>? GetTopicList()
        {
            if (TopicList is null)
                TopicList = UserQuery?.GetTopics();
            return TopicList;
        }
        public bool UserQuerysExists(int ALevel)
        {
            var filtered = GetTopicList()?.Where(p => p.Parent_Id == ALevel).ToList();
            return filtered?.Count > 0;
        }
        public string GetTopicName(int AId)
        {
            var filtered = GetTopicList()?.Where(p => p.Id == AId).ToList();
            if (filtered?.Count > 0)
                return filtered[0].Name ?? "";
            return "";
        }
        private async Task ShowUserButtons(ITelegramBotClient AbotClient, Update? Aupdate, int ALevel)
        {
            if (!UserQuerysExists(ALevel))
                return;
            Dictionary<long, string> keyboardList = new();
            GetTopicList();
            if (TopicList is not null)
                foreach (var r in TopicList)
                    if (r.Parent_Id == ALevel)
                        keyboardList.Add((int)r.Id, r?.Name ?? "");

            if (Aupdate?.Message is not null)
                await AbotClient.SendMessage(Aupdate.Message.Chat.Id, "Выберите раздел:", replyMarkup:
                                      HandleUpdatesUtils.GetKeyBoard(keyboardList, _userquerysbord));
            else
            if (Aupdate?.CallbackQuery?.Message is not null)
                await AbotClient.SendMessage(Aupdate.CallbackQuery.Message.Chat.Id, "Выберите раздел:", replyMarkup:
                                      HandleUpdatesUtils.GetKeyBoard(keyboardList, _userquerysbord));
        }
                
        protected override async Task UpdateReceivedStart(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            if (Aupdate?.Message?.Text == string.Empty || Aupdate?.Message?.Text?[0] == '\0')
                return;
            var vuser = UserQuery?.SelectUserByIdent(Aupdate?.Message?.From?.Id);
            if (vuser?.Roles_id == RolesEnum.reUnknown)
                vuser.Roles_id = RolesEnum.reUser;
            if (vuser?.Roles_id != RolesEnum.reUser)
                return;

            if (Aupdate?.Message?.Text?[0] != '/')
            {
                if (UserQuery != null)
                {
                    if (Aupdate?.Message?.Chat != null)
                    {
                        var vHasOpenedMessages = UserQuery.HasOpenedMessages(vuser?.Id);
                        UserQuery.AddMessage(vuser?.Id, Aupdate?.Message?.Text, vuser?.Topic_id, null);
                        long? answerer_ident = UserQuery.GetAnswererIdent(vuser?.Id);
                        if (answerer_ident != null && answerer_ident != 0)
                            await AbotClient.SendMessage(answerer_ident, Aupdate?.Message?.Text ?? "");
                        if (Aupdate?.Message?.From is not null)
                            if (!vHasOpenedMessages)
                                await AbotClient.SendMessage(Aupdate.Message.Chat.Id, $"Оператор ответит вам в ближайшее время");
                    }
                }
            }


        }
                
        protected override async Task UpdateCallBackKeyboard(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            if (Aupdate?.CallbackQuery?.Message is not null)
            {
                var strs = Aupdate.CallbackQuery.Data?.Split('.');
                if ((strs?.Length > 1) && (BotSession is not null))
                {
                    switch (strs[0])
                    {                        
                        case _userquerysbord:
                            await HandleUpdatesUtils.HideInlineKeyboard(AbotClient, Aupdate);
                            var vLevel = int.Parse(strs[1]);
                            if (UserQuerysExists(vLevel))
                            {
                                await AbotClient.SendMessage(Aupdate.CallbackQuery.Message.Chat.Id, $"Ваш раздел {GetTopicName(vLevel)}");
                                await ShowUserButtons(AbotClient, Aupdate, vLevel);
                            }
                            else
                            {
                                UserQuery?.SetUserTopicId(Aupdate?.CallbackQuery?.From?.Id, vLevel);
                                DoConShowMessage($"раздел {GetTopicName(vLevel)}");
                                if (Aupdate is not null)
                                    await AbotClient.SendMessage(Aupdate.CallbackQuery.Message.Chat.Id, $"Ваш раздел {GetTopicName(vLevel)}, задайте свой вопрос");
                            }
                            break;

                        default:
                            break;
                    }
                }

            }
        }
    }
}
