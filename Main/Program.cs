namespace DefaultNamespace;

public class Program
{
    static void Main(string[] args)
    {
        //1.) Create project
        var project = new Project(1, "Archon PMS Launch", "Console MVP of Archon project manager", "Lead Developer");
        
        //2.) Add Staff
        var dev1 = new Staff(101, "Karden Lewis");
        var dev2 = new Staff(102, "Andrew London");
        var dev3 = new Staff(103, "Corwin");
        var dev4 = new Staff(104, "Julien M");
        
        //3.) Add Risks
        project.Risklist.Add(new Risk("Scope Creep", "Adding features beyond specifications", "Project Management",
            RiskPriority.High));
        //4.) Add Requirements
        project.Requirementlist.Add(new Requirement(1, "Console Menu System", "Interactive text menu to manage tasks",
            RequirementCategory.Functional));
        //5.) Add Deliverables
        project.DeliverableList.Add(new Deliverable(1, "Core Data Models", "Define entities for project, staff, risks",
            DateTime.Now.AddDays(7), dev1));
        
        //6.) Console Output 
        project.DisplayOverview();
    }
}