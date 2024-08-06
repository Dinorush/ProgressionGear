namespace ProgressionGear.ProgressionLock
{
    public sealed class ProgressionData
    {
        public string ExpeditionKey { get; set; } = string.Empty;

        public int MainCompletionCount { get; set; }

        public int SecondaryCompletionCount { get; set; }

        public int ThirdCompletionCount { get; set; }

        public int AllCompletionCount { get; set; }

        public int NoBoosterAllClearCount { get; set; }

        public ProgressionData() { }

        public ProgressionData(string key, int main, int secondary, int third, int all, int noBooster)
        {
            ExpeditionKey = key;
            MainCompletionCount = main;
            SecondaryCompletionCount = secondary;
            ThirdCompletionCount = third;
            AllCompletionCount = all;
            NoBoosterAllClearCount = noBooster;
        }
    }
}
