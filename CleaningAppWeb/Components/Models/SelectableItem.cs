namespace CleaningAppWeb.Components.Models
{
    public abstract class SelectableItem
    {
        public abstract string GetDisplayInfo();

        public virtual bool MatchesSearch(string searchText)
            => GetDisplayInfo().Contains(searchText, StringComparison.OrdinalIgnoreCase);

        public virtual object GetId() => GetHashCode();

        public override bool Equals(object? obj)
        {
            if (obj is not SelectableItem other) return false;
            return GetId().Equals(other.GetId());
        }

        public override int GetHashCode() => GetId().GetHashCode();
    }
}
