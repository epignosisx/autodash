using System;
using System.Collections.Generic;

namespace Autodash.Core
{
    public class Browser : IEquatable<Browser>, IComparer<Browser>, IComparable<Browser>
    {
        public string Name { get; set; }
        public string Version { get; set; }

        public Browser()
        {
        }

        public Browser(string name) : this(name, null)
        {
        }

        public Browser(string name, string version)
        {
            Name = name;
            Version = version;
        }

        public bool Equals(Browser other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name) && string.Equals(Version, other.Version);
        }

        public int Compare(Browser x, Browser y)
        {
            int comp = string.Compare(x.Name, y.Name, StringComparison.Ordinal);
            if (comp != 0)
                return comp;

            return string.Compare(x.Version, y.Version, StringComparison.Ordinal);
        }

        public int CompareTo(Browser other)
        {
            return Compare(this, other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Browser) obj);
        }

        public static bool operator ==(Browser left, Browser right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Browser left, Browser right)
        {
            return !Equals(left, right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0)*397) ^ (Version != null ? Version.GetHashCode() : 0);
            }
        }



        public override string ToString()
        {
            if (string.IsNullOrEmpty(Version))
                return Name;
            return Name + " " + Version;
        }
    }
}