namespace ThColumnInfo.Validate
{
    public abstract class ValidateModel
    {
        public string Code { get; set; }
        public virtual bool ValidateProperty()
        {
            return true;
        }
    }
}
