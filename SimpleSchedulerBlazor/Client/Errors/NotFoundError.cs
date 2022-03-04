namespace SimpleSchedulerBlazor.Client.Errors;

public record class NotFoundError(string Message) : Error(Message);
