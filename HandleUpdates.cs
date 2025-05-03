using MyTelegramBot.DapperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using MyTelegramBot.Classes;

namespace MyTelegramBot
{
    internal class HandleUpdates : CustomHandleUpdates
    {
        //static List<CustomRole> RoleList;
        public static bool BeginUpdate = true;

        public static void RegisterHandlesUpdates(ref UpdateReceivedDelegate? updRecDelegate)
        {
            updRecDelegate += UpdateReceivedBegin;
            updRecDelegate += UpdateReceivedStart;
            updRecDelegate += UpdateReceivedRole;
        }

        public static void UpdateReceivedBegin(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            BeginUpdate = false;
        }

        /*public static class TelegramKeyboardHelper
        {
            public static InlineKeyboardMarkup CreateKeyboard(Dictionary<string, string> buttonData)
            {
                // Создаем список строк кнопок
                var keyboardRows = new List<List<InlineKeyboardButton>>();
                var currentRow = new List<InlineKeyboardButton>();

                // Проходим по словарю, где ключ - метка, значение - callbackData
                foreach (var button in buttonData)
                {
                    // Создаем кнопку с меткой (text) и значением (callback_data)
                    var inlineKeyboardButton = InlineKeyboardButton.WithCallbackData(button.Key, button.Value);
                    currentRow.Add(inlineKeyboardButton);
                }

                // Добавляем строку кнопок в список строк
                keyboardRows.Add(currentRow);

                // Создаем объект InlineKeyboardMarkup из списка строк
                return new InlineKeyboardMarkup(keyboardRows);
            }
        }
        public static async Task SendMessageWithButtons(ITelegramBotClient botClient, long chatId)
        {
            // Определяем данные для кнопок. Ключ - текст на кнопке, значение - данные для callback
            var buttonData = new Dictionary<string, string>
    {
        { "Кнопка 1", "value_1" },
        { "Кнопка 2", "value_2" },
        { "Кнопка 3", "value_3" }
    };

            // Создаем клавиатуру
            var inlineKeyboard = TelegramKeyboardHelper.CreateKeyboard(buttonData);

            // Отправляем сообщение с клавиатурой
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Выберите опцию:",
                replyMarkup: inlineKeyboard);
        }*/

        public static void UpdateReceivedStart(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            if (BeginUpdate) return;

            void SelectRole1(List<CustomRole>  ARoleList)
            {
                var keyboard = new ReplyKeyboardMarkup();
                keyboard.ResizeKeyboard = true;

                var info = "Выберите доступную роль";

                foreach (var item in ARoleList)
                {
                    //var button = new KeyboardButton($"{item.Id}.{item.Name}");// { RequestUsers = "param1=value1" };                    
                    var button = new KeyboardButton($"{item.Id}.{item.Name}");// { RequestUsers = "param1=value1" };                    
                    keyboard.AddButton(button);
                }


                AbotClient.SendMessage(Aupdate.Message.Chat.Id, info, replyMarkup: keyboard);

            };

            

            void SelectRole(List<CustomRole> ARoleList)
            {
                var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(new[]
                {
                    new [] // first row
                    {
                        InlineKeyboardButton.WithUrl("1.1","www.google.com"),
                        InlineKeyboardButton.WithCallbackData("1.2"),
                    },
                    new [] // second row
                    {
                        InlineKeyboardButton.WithCallbackData("2.1"),
                        InlineKeyboardButton.WithCallbackData("2.2"),
                    }
                });
                //AbotClient.SendTextMessageAsync(Aupdate.Message.Chat.Id, "Жамкни!", replyMarkup: keyboard);
            }

            if (Aupdate.Message.Text == "/start")
            {
                var info = $"{Aupdate.Message.From.Username}({Aupdate.Message.From.Id})({Aupdate.Message.From.FirstName} {Aupdate.Message.From.LastName}) : {Aupdate.Message.Text}";
                DoConShowMessage(info);

                var vuser = Query.SelectUser(Aupdate.Message.From.Id);
                var vUserId = vuser.Id;
                if (vuser == null)
                {
                    vuser = new CustomUser();
                    vuser.UserName = Aupdate.Message.From.Username;
                    vuser.User_Ident = Aupdate.Message.From.Id;
                    vuser.FirstName = Aupdate.Message.From.FirstName;
                    vuser.LastName = Aupdate.Message.From.LastName;
                    vuser.Roles_id = 0;
                    vUserId = Query.InsertUser(vuser);
                }
                //if (RoleList is null)
                var vRoleList = Query.SelectRoles(vUserId);

                if (vRoleList.Count > 1)
                    SelectRole(vRoleList);
                else
                    DoGetMenuRole(AbotClient, Aupdate, vRoleList[0].Id);

                BeginUpdate = true;
                return;
            }
            BeginUpdate = false;
        }

        protected static void DoGetMenuRole(ITelegramBotClient AbotClient, Update Aupdate, long ARole_Id)
        {
            void ShowUserMenu0()
            {
                var buttons = new ReplyKeyboardMarkup();
                buttons.ResizeKeyboard = true;

                var info = "Выберите тему";

                buttons.AddButton(new KeyboardButton($"Ресторан"));
                buttons.AddButton(new KeyboardButton($"Тихий час"));
                buttons.AddButton(new KeyboardButton($"Трансфер"));
                AbotClient.SendMessage(Aupdate.Message.Chat.Id, info, replyMarkup: buttons);


            }

            if (ARole_Id == 0)
            {
                ShowUserMenu0();
            }
        }
        public static void UpdateReceivedRole(ITelegramBotClient AbotClient, Update Aupdate, CancellationToken Atoken)
        {
            if (BeginUpdate) return;
            /*if (RoleList is null) return;
            bool HasRole()
            {
                string[] vArrWord = Aupdate.Message.Text.Split('.', StringSplitOptions.None);
                int vId = 0;

                if ((vArrWord.Length > 1) && (int.TryParse(vArrWord[0], out vId)))
                {
                    foreach (var vRole in RoleList)
                    {
                        if ((vRole.Id == vId) && (vRole.Name == vArrWord[1]))
                            return true;
                    }
                    return false;
                }
                return false;*/

            int ARole_Id = 0;
            bool HasRole()
            {
                string[] vArrWord = Aupdate.Message.Text.Split('.', StringSplitOptions.None);

                if ((vArrWord.Length > 1) && (int.TryParse(vArrWord[0], out ARole_Id)))
                {
                    var vuser = Query.SelectUser(Aupdate.Message.From.Id);
                    return (Query.HasRole(vuser.Id, ARole_Id));
                }
                return false;
            }

            if (HasRole())
            {
                if (ARole_Id == 0)
                {
                    DoGetMenuRole(AbotClient, Aupdate, ARole_Id);
                }
                BeginUpdate = true;
            }
            else
            {
                DoConShowMessage("NoHas");
                BeginUpdate = false;
            }

        }
        


    }
}
