namespace Systems.CheatConsole
{
    /// <summary>
    /// Represents the result of executing a cheat command.
    /// </summary>
    public sealed class CheatConsoleResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheatConsoleResult"/> class.
        /// </summary>
        /// <param name="success">Whether the command execution was successful.</param>
        /// <param name="message">The associated message (info, error, etc.).</param>
        /// <param name="messageType">The type of message to display in the console.</param>
        /// <param name="originalInput">The original command text entered by the user.</param>
        public CheatConsoleResult(bool success, string message, CheatConsoleMessageType messageType, 
            string originalInput)
        {
            Success = success;
            Message = message;
            MessageType = messageType;
            OriginalInput = originalInput;
        }

        /// <summary>
        /// Gets a value indicating whether execution was successful.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the descriptive message resulting from execution.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the message type to display.
        /// </summary>
        public CheatConsoleMessageType MessageType { get; }

        /// <summary>
        /// Gets the original command text entered by the user.
        /// </summary>
        public string OriginalInput { get; }
    }
}