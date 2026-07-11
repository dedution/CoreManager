namespace core.modules
{
    public interface ILocale
    {
        string LocaleID { get; set;}

        void UpdateLocalization();
    }
}