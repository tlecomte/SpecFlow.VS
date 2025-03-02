﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SpecFlow.VisualStudio.Common;

namespace SpecFlow.VisualStudio.Configuration
{
    public class TagLinkConfiguration
    {
        public string TagPattern { get; set; }
        public string UrlTemplate { get; set; }

        internal Regex ResolvedTagPattern { get; private set; }

        private void FixEmptyContainers()
        {
            //nop;
        }

        public void CheckConfiguration()
        {
            FixEmptyContainers();

            if (string.IsNullOrEmpty(TagPattern))
                throw new DeveroomConfigurationException("'traceability/tagLinks[]/tagPattern' must be specified");
            if (string.IsNullOrEmpty(UrlTemplate))
                throw new DeveroomConfigurationException("'traceability/tagLinks[]/urlTemplate' must be specified");

            try
            {
                ResolvedTagPattern = new Regex("^" + TagPattern.TrimStart('^').TrimEnd('$') + "$");
            }
            catch (Exception e)
            {
                throw new DeveroomConfigurationException($"Invalid regular expression '{TagPattern}' was specified as 'traceability/tagLinks[]/tagPattern': {e.Message}");
            }
        }

        #region Equality

        protected bool Equals(TagLinkConfiguration other)
        {
            return string.Equals(TagPattern, other.TagPattern) && string.Equals(UrlTemplate, other.UrlTemplate) && Equals(ResolvedTagPattern, other.ResolvedTagPattern);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TagLinkConfiguration) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (TagPattern != null ? TagPattern.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (UrlTemplate != null ? UrlTemplate.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ResolvedTagPattern != null ? ResolvedTagPattern.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}
