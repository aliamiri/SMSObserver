namespace TelerikWpfApp2
{
    public class RevcieveSms
    {
        public RevcieveSms(int id,long delay, int type, string typeName)
        {
            smsId = id;
            Delay = delay;
            Type = type;
            TypeName = typeName;
        }

        public int smsId { get; set; }

        public long Delay { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; }
    }
}