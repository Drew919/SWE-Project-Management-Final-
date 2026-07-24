namespace ArchonPM.Objects
{
    public class Requirement
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        /// <summary>Functional or Non-Functional. Existing field name retained.</summary>
        public string Catagory { get; set; } = "Functional";
    }
}
