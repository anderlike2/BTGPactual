namespace BTGPactual.Shared.Constants;

public static class AppConstants
{
    public static class DefaultValues
    {
        public const decimal InitialBalance = 500000m;
        public const string AdminEmail = "anderlike4@gmail.com";
        public const string AdminPassword = "Admin1980*.";
    }

    public static class ErrorMessages
    {
        public const string UserNotFound = "Usuario no encontrado";
        public const string UserNotActive = "El usuario no está activo";
        public const string UserAlreadyExists = "El usuario ya existe";
        public const string InvalidCredentials = "Credenciales inválidas";
        public const string Unauthorized = "No autorizado";

        public const string FundNotFound = "Fondo no encontrado";
        public const string FundNotActive = "El fondo no está activo";
        public const string FundAlreadyExists = "El fondo ya existe";

        public const string TransactionNotFound = "Transacción no encontrada";
        public const string AlreadySubscribed = "Ya está suscrito a este fondo";
        public const string NotSubscribed = "No está suscrito a este fondo";
        public const string InsufficientAmount = "El monto debe ser al menos el mínimo del fondo";
        public const string InvalidAmount = "El monto debe ser mayor a cero";
        public const string CannotCancelSubscription = "No se puede cancelar la suscripción";
        public const string ValidationError = "Errores de validación";
        public const string InternalServerError = "Ha ocurrido un error interno en el servidor";
    }

    public static class ValidationMessages
    {
        public const string EmailRequired = "El email es requerido";
        public const string EmailInvalid = "El formato del email es inválido";

        public const string UsernameRequired = "El nombre de usuario es requerido";
        public const string UsernameMinLength = "El nombre de usuario debe tener al menos 3 caracteres";
        public const string UsernameMaxLength = "El nombre de usuario no puede exceder 50 caracteres";
        public const string PasswordRequired = "La contraseña es requerida";
        public const string PasswordMinLength = "La contraseña debe tener al menos 8 caracteres";
        public const string FirstNameRequired = "El nombre es requerido";
        public const string FirstNameMaxLength = "El nombre no puede exceder 50 caracteres";
        public const string LastNameRequired = "El apellido es requerido";
        public const string LastNameMaxLength = "El apellido no puede exceder 50 caracteres";
        public const string PhoneNumberInvalid = "El número de teléfono no tiene un formato válido (E.164)";
        public const string InvalidNotificationPreference = "La preferencia de notificación no es válida";

        public const string FundIdRequired = "El ID del fondo es requerido";
        public const string FundNameRequired = "El nombre del fondo es requerido";
        public const string FundNameMaxLength = "El nombre del fondo no puede exceder 100 caracteres";
        public const string MinimumAmountMustBeGreaterThanZero = "El monto mínimo debe ser mayor a 0";
        public const string InvalidFundCategory = "La categoría del fondo no es válida";

        public const string AmountMustBeGreaterThanZero = "El monto debe ser mayor a 0";

        public const string InvalidObjectId = "El ID no es válido";
        public const string RequiredField = "Este campo es requerido";
    }

    public static class SuccessMessages
    {
        public const string OperationSuccessful = "Operación exitosa";

        public const string RegistrationSuccessful = "Usuario registrado exitosamente";
        public const string LoginSuccessful = "Inicio de sesión exitoso";

        public const string FundCreated = "Fondo creado exitosamente";
        public const string FundUpdated = "Fondo actualizado exitosamente";

        public const string SubscriptionSuccessful = "Suscripción realizada exitosamente";
        public const string CancellationSuccessful = "Cancelación realizada exitosamente";
    }

    public static class NotificationMessages
    {
        public const string SubscriptionEmailSubject = "Confirmación de Suscripción - BTG Pactual";
        public const string CancellationEmailSubject = "Confirmación de Cancelación - BTG Pactual";

        public const string SubscriptionEmailTitle = "Suscripción Exitosa";
        public const string CancellationEmailTitle = "Cancelación Exitosa";

        public const string SubscriptionEmailBody = "Su suscripción al fondo <strong>{0}</strong> ha sido procesada exitosamente.";
        public const string SubscriptionEmailAmount = "Monto vinculado: <strong>{0}</strong>";
        public const string CancellationEmailBody = "Su cancelación del fondo <strong>{0}</strong> ha sido procesada exitosamente.";
        public const string CancellationEmailAmount = "Monto devuelto: <strong>{0}</strong>";

        public const string EmailFooter = "Gracias por confiar en BTG Pactual.";
        public const string EmailDisclaimer = "Este es un mensaje automático, por favor no responder.";

        public const string SubscriptionSmsTemplate = "BTG Pactual: Suscripción exitosa al fondo {0} por {1}. Gracias por su confianza.";
        public const string CancellationSmsTemplate = "BTG Pactual: Cancelación exitosa del fondo {0}. Monto devuelto: {1}. Gracias.";
    }
}
