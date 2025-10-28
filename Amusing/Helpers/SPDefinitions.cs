namespace Amusing.Helpers;

public class SPDefinitions
{
    public static readonly string RunGetUsersLog = @"CALL getuserslog();";
    public static readonly string RunGetPersonsLog = @"CALL getpersonslog();";

    public static readonly string RunGetPaymentsLog = @"CALL getpayments();";

    public static readonly string RunGetAllUserLoginsLog = @"CALL getuserlogins(null);";        // Get all user logins
    public static readonly string RunGetUserLoginsLog = @"CALL getuserlogins(365);";            // Only get the user logins for the last 365 days
    public static readonly string RunGetAllPersonLoginsLog = @"CALL getpersonlogins(null);";    // Get all person logins
    public static readonly string RunGetPersonLoginsLog = @"CALL getpersonlogins(365);";        // Only get the person logins for the last 365 days
}
