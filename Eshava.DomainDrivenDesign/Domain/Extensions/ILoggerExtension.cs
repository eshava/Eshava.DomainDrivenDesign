using Eshava.DomainDrivenDesign.Application.Settings;
using Eshava.DomainDrivenDesign.Domain.Dtos;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;

namespace Eshava.DomainDrivenDesign.Domain.Extensions
{
	public static class ILoggerExtension
    {
        public static Guid LogTrace<T>(this ILogger logger, T source, AbstractScopedSettings scopedSettings, string message, object additional = null, Guid? messageGuid = null, [CallerMemberName] string memberName = null, [CallerLineNumber] int lineNumber = 0) where T : class
        {
            return Log(logger, LogLevel.Trace, source, scopedSettings, memberName, lineNumber, message, null, additional, messageGuid);
        }

        public static Guid LogDebug<T>(this ILogger logger, T source, AbstractScopedSettings scopedSettings, string message, object additional = null, Guid? messageGuid = null, [CallerMemberName] string memberName = null, [CallerLineNumber] int lineNumber = 0) where T : class
        {
            return Log(logger, LogLevel.Debug, source, scopedSettings, memberName, lineNumber, message, null, additional, messageGuid);
        }

        public static Guid LogInformation<T>(this ILogger logger, T source, AbstractScopedSettings scopedSettings, string message, object additional = null, Guid? messageGuid = null, [CallerMemberName] string memberName = null, [CallerLineNumber] int lineNumber = 0) where T : class
        {
            return Log(logger, LogLevel.Information, source, scopedSettings, memberName, lineNumber, message, null, additional, messageGuid);
        }

        public static Guid LogWarning<T>(this ILogger logger, T source, AbstractScopedSettings scopedSettings, string message, Exception exception = null, object additional = null, Guid? messageGuid = null, [CallerMemberName] string memberName = null, [CallerLineNumber] int lineNumber = 0) where T : class
        {
            return Log(logger, LogLevel.Warning, source, scopedSettings, memberName, lineNumber, message, exception, additional, messageGuid);
        }

        public static Guid LogError<T>(this ILogger logger, T source, AbstractScopedSettings scopedSettings, string message, Exception exception = null, object additional = null, Guid? messageGuid = null, [CallerMemberName] string memberName = null, [CallerLineNumber] int lineNumber = 0) where T : class
        {
            return Log(logger, LogLevel.Error, source, scopedSettings, memberName, lineNumber, message, exception, additional, messageGuid);
        }

        public static Guid LogCritical<T>(this ILogger logger, T source, AbstractScopedSettings scopedSettings, string message, Exception exception = null, object additional = null, Guid? messageGuid = null, [CallerMemberName] string memberName = null, [CallerLineNumber] int lineNumber = 0) where T : class
        {
            return Log(logger, LogLevel.Critical, source, scopedSettings, memberName, lineNumber, message, exception, additional, messageGuid);
        }

        private static Guid Log<T>(ILogger logger, LogLevel logLevel, T source, AbstractScopedSettings scopedSettings, string memberName, int lineNumber, string message, Exception exception, object additional, Guid? messageGuid) where T : class
        {
            return Log(logger, logLevel, source.GetType(), scopedSettings, memberName, lineNumber, message, exception, additional, messageGuid);
        }

        private static Guid Log(ILogger logger, LogLevel logLevel, Type sourceType, AbstractScopedSettings scopedSettings, string memberName, int lineNumber, string message, Exception exception, object additional, Guid? messageGuid)
        {
            if (!messageGuid.HasValue || messageGuid.Value == Guid.Empty)
            {
                messageGuid = Guid.NewGuid();
            }

            var additionalInformation = new LogInformationDto
            {
                Message = message,
                Class = sourceType.Name,
                Method = memberName,
                LineNumber = lineNumber,
                Information = additional,
                ScopedInformation = scopedSettings?.GetScopeInformationForLogging(),
                MessageGuid = messageGuid
            };

            logger.Log(logLevel, new EventId(0), additionalInformation, exception, (state, ex) => ex?.Message);

            return messageGuid.Value;
        }
    }
}