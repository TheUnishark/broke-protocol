﻿using BrokeProtocol.API;
using BrokeProtocol.Entities;
using BrokeProtocol.Required;
using BrokeProtocol.GameSource.Types;
using System;

namespace BrokeProtocol.CustomEvents
{
    public class ExampleCommand : IScript
    {
        public const string coinFlip = "coinflip";
        public const string heads = "heads";
        public const string tails = "tails";
        public const string cancel = "cancel";

        public ExampleCommand()
        {
            CommandHandler.RegisterCommand("ClearCrimes", new Action<ShPlayer, ShPlayer>(ClearCrimes), null, "example.clearcrimes");

            CommandHandler.RegisterCommand("Example", new Action<ShPlayer, ShPlayer, byte, int, float, string, string>(Command1), (player, command) =>
            {
                // Silly example
                if (player.health < 50f)
                {
                    player.svPlayer.SendGameMessage("Must be over 50 health to use this command");
                    return false;
                }
                return true;
            });

            CommandHandler.RegisterCommand("ExampleInt", new Action<ShPlayer, int>(Command2), null, "example.int");
            CommandHandler.RegisterCommand("ExampleString", new Action<ShPlayer, string>(Command3), null, "example.string");
            CommandHandler.RegisterCommand("ExamplePlayer", new Action<ShPlayer, ShPlayer>(Command4), null, "example.player");
            CommandHandler.RegisterCommand("ExampleDiscord", new Action<ShPlayer>(Command5), null, "example.discord");
            CommandHandler.RegisterCommand("CoinFlip", new Action<ShPlayer>(CoinFlip), null, "example.coinflip");
        }

        public void ClearCrimes(ShPlayer player, ShPlayer target = null)
        {
            if (!target) target = player;

            if (Manager.pluginPlayers.TryGetValue(target, out var pluginPlayer))
            {
                pluginPlayer.ClearCrimes();
            }
        }

        // Any optional parameters here will be optional with in-game commands too
        public void Command1(ShPlayer player, ShPlayer target, byte byte1 = 1, int int1 = 2, float float1 = 3f, string string1 = "default1", string string2 = "default2")
        {
            player.svPlayer.SendGameMessage($"'{target.username}' '{byte1}' '{int1}' '{float1}' '{string1}' '{string2}'");
        }

        public void Command2(ShPlayer player, int i = 1)
        {
            player.svPlayer.SendGameMessage("int : " + i);
        }

        public void Command3(ShPlayer player, string s = "testString")
        {
            player.svPlayer.SendGameMessage("string : " + s);
        }

        public void Command4(ShPlayer player, ShPlayer p)
        {
            player.svPlayer.SendGameMessage("player : " + p.username);
        }

        public void Command5(ShPlayer player)
        {
            player.svPlayer.SvOpenURL("https://discord.gg/wEB2ZGU", "Open Official BP Discord");
        }

        public void CoinFlip(ShPlayer player)
        {
            player.svPlayer.SendTextPanel("Pick Heads or Tails", coinFlip, new LabelID[] { new LabelID("Heads", heads), new LabelID("Tails", tails), new LabelID("Cancel", cancel)});
        }
    }
}
