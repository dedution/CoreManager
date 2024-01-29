namespace core.modules
{
    public interface ISaver
    {
        void SaveSystem_Load();
        void SaveSystem_Save();

        string GetGUID();

        object SaveData { get; }
        bool SaveSystem_Enabled { get; }
        string SaveSystem_GUID  { get; }
    }
}