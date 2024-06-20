using Amazon.DynamoDBv2.DataModel;
namespace NextCloudBackendServices.Models;

[DynamoDBTable("NextCloud-users")]
public class User
{
    [DynamoDBHashKey("user_id")]
    public string? Id { get; set; }
    
    [DynamoDBProperty("username")]
    public string? UserName { get; set; }
    
    [DynamoDBRangeKey("email")]
    
    public string? Email { get; set; }
    
    [DynamoDBProperty("password")]
    public string? Password { get; set; }
}

public class UserLogin
{
    [DynamoDBProperty("email")]
    
    public string? Email { get; set; }
    
    [DynamoDBProperty("password")]
    public string? Password { get; set; }
}