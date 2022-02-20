namespace SimpleSchedulerModels.ApiModels.Login;

public record class GetAllUserEmailsRequest();
public record class GetAllUserEmailsResponse(ImmutableArray<string> EmailAddresses);
