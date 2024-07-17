namespace QuestAWAY
{
    internal class MapIcon
    {
        public string Name { get; }
        public Category Category { get; }

        public MapIcon(string name, Category category)
        {
            Name = name;
            Category = category;
        }
    }
}
