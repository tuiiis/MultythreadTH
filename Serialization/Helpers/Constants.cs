namespace Serialization.Helpers
{
    public static class Constants
    {
        public const string XmlFilePath = "objects.xml";
        public static readonly string[] ObjectTypes = { "Manufacturer", "Tank" };

        // Menu options
        public const string MenuPrompt = "Choose an option:";
        public const string MenuOption1 = "1. Create 10 Manufacturers and 10 Tanks";
        public const string MenuOption2 = "2. Save objects to XML";
        public const string MenuOption3 = "3. Display XML as text";
        public const string MenuOption4 = "4. Parse XML to objects";
        public const string MenuOption5 = "5. Extract 'Model' values with XDocument";
        public const string MenuOption6 = "6. Extract 'Model' values with XMLDocument";
        public const string MenuOption7 = "7. Edit attribute by field name (XDocument)";
        public const string MenuOption8 = "8. Edit attribute by field name (XMLDocument)";
        public const string MenuOption9 = "9. Exit program";

        // Messages
        public const string CreateInstancesFirst = "Create instances first (1.)";
        public const string ObjectsSavedToXml = "Objects saved to XML.";
        public const string NoXmlFileFound = "No XML file found.";
        public const string ExitingProgram = "Exiting program.";
        public const string InvalidChoice = "Invalid choice. Please select a valid option.";
        public const string ManufacturersCreated = "Manufacturers created:";
        public const string TanksCreated = "Tanks created:";
        public const string TankModelValuesXDocument = "Tank Model values (XDocument):";
        public const string TankModelValuesXmlDocument = "Tank Model values (XMLDocument):";
        public const string EnterObjectType = "Enter object type ({0}):";
        public const string InvalidObjectType = "Invalid object type. Must be one of: {0}";
        public const string NoObjectsFoundInXml = "No {0} objects found in XML.";
        public const string AvailableObjects = "Available {0} objects:";
        public const string EnterObjectNumberToEdit = "Enter {0} number to edit (1-{1}):";
        public const string InvalidNumber = "Invalid number.";
        public const string SelectedNodeIsNull = "Selected node is null.";
        public const string EnterFieldNameToEdit = "Enter field name to edit:";
        public const string FieldNameCannotBeEmpty = "Field name cannot be empty.";
        public const string FieldNotFound = "Field '{0}' not found in {1}.";
        public const string CurrentValueOfField = "Current value of {0}: {1} (Type: {2})";
        public const string EnterNewValue = "Enter new value:";
        public const string UpdatedField = "Updated {0} to {1}";
        public const string InvalidValueForType = "Invalid value for type {0}";
        // Add XML element constants
        public const string Element_Root = "Root";
        public const string Element_Manufacturers = "Manufacturers";
        public const string Element_Manufacturer = "Manufacturer";
        public const string Element_Name = "Name";
        public const string Element_Address = "Address";
        public const string Element_IsAChildCompany = "IsAChildCompany";
        public const string Element_Tanks = "Tanks";
        public const string Element_Tank = "Tank";
        public const string Element_ID = "ID";
        public const string Element_Model = "Model";
        public const string Element_SerialNumber = "SerialNumber";
        public const string Element_TankType = "TankType";

    }
}
