
using System.Collections.Generic;
using Fabron.Contracts;
using HoneyComb.Commands;

namespace HoneyComb.API
{
    public static class FabronExtensions
    {
        public static string? GetLabel(this Job<InvokeHttpRequest, InvokeHttpRequestResult> job, string labelName)
        {
            if (job.Metadata.Labels == null)
            {
                return null;
            }

            if (job.Metadata.Labels.TryGetValue(labelName, out var value))
            {
                return value;
            }
            return null;
        }

        public static string RequireLabel(this Job<InvokeHttpRequest, InvokeHttpRequestResult> job, string labelName)
        {
            var value = job.GetLabel(labelName);
            if (value == null)
            {
                throw new KeyNotFoundException($"Label {labelName} not found in {job.Metadata.Key}");
            }

            return value;
        }

        public static string? GetAnnotation(this Job<InvokeHttpRequest, InvokeHttpRequestResult> job, string annoName)
        {
            if (job.Metadata.Annotations == null)
            {
                return null;
            }

            if (job.Metadata.Annotations.TryGetValue(annoName, out var value))
            {
                return value;
            }
            return null;
        }

        public static string RequireAnnotation(this Job<InvokeHttpRequest, InvokeHttpRequestResult> job, string name)
        {
            var value = job.GetAnnotation(name);
            if (value == null)
            {
                throw new KeyNotFoundException($"Annotation {name} not found in {job.Metadata.Key}");
            }

            return value;
        }

        public static string? GetLabel(this CronJob<InvokeHttpRequest> cron, string labelName)
        {
            if (cron.Metadata.Labels == null)
            {
                return null;
            }

            if (cron.Metadata.Labels.TryGetValue(labelName, out var value))
            {
                return value;
            }
            return null;
        }

        public static string RequireLabel(this CronJob<InvokeHttpRequest> cron, string labelName)
        {
            var value = cron.GetLabel(labelName);
            if (value == null)
            {
                throw new KeyNotFoundException($"Label {labelName} not found in {cron.Metadata.Key}");
            }

            return value;
        }

        public static string? GetAnnotation(this CronJob<InvokeHttpRequest> cron, string annoName)
        {
            if (cron.Metadata.Annotations == null)
            {
                return null;
            }

            if (cron.Metadata.Annotations.TryGetValue(annoName, out var value))
            {
                return value;
            }
            return null;
        }

        public static string RequireAnnotation(this CronJob<InvokeHttpRequest> cron, string name)
        {
            var value = cron.GetAnnotation(name);
            if (value == null)
            {
                throw new KeyNotFoundException($"Annotation {name} not found in {cron.Metadata.Key}");
            }

            return value;
        }

    }

}
