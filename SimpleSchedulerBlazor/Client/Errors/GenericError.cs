namespace SimpleSchedulerBlazor.Client.Errors;

public record class GenericError(string Message) : Error(Message);
