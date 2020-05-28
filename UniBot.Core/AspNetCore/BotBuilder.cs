using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniBot.Core.Abstraction;
using UniBot.Core.Actions;

namespace UniBot.Core.AspNetCore
{
    public class BotBuilder : IBotBuilder
    {
        private Bot _bot;
        
        public BotBuilder(Bot bot)
        {
            _bot = bot;
        }

        public IBotBuilder DetectCommands()
        {
            // TODO Find an elegant solution.
            var commands = AppDomain.CurrentDomain
                                    .GetAssemblies()
                                    .SelectMany(ass => ass.GetTypes())
                                    .Where(x => string.CompareOrdinal(typeof(CommandBase).ToString(), x?.BaseType?.ToString() ?? string.Empty) == 0 
                                                && !x.IsInterface 
                                                && !x.IsAbstract)
                                    .Select(t => (CommandBase) Activator.CreateInstance(t)!);

            foreach (var command in commands)
                _bot.RegisterCommand(command);

            return this;
        }
    }
}