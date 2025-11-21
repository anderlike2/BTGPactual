# BTG Pactual - Sistema de Gestión de Fondos de Inversión

Sistema backend desarrollado en .NET 10 para la gestión de fondos de inversión, permitiendo a los usuarios suscribirse y cancelar suscripciones a diferentes fondos (FPV y FIC), con notificaciones automáticas vía email y SMS.

## **Tecnologías principales**

- Framework: .NET Core 10.0
- Base de datos: MongoDB (conector oficial)
- Autenticación: JWT Bearer (tokens JSON Web Token)
- Validaciones: FluentValidation
- Email/SMS: Amazon SES (email) y SNS (SMS) – implementaciones Fake incluidas para pruebas
- Testing: Pruebas unitarias e integración con xUnit y Moq
- Docker: Se incluye un Dockerfile para creación de imagen

## **Características principales**

El sistema implementa las siguientes funcionalidades principales:

1. **Autenticación y Autorización**
   - Registro de usuarios con validación
   - Login con JWT
   - Control de acceso basado en roles (Admin/Client)

2. **Gestión de Fondos**
   - CRUD completo de fondos
   - Categorización (FPV/FIC)
   - Control de montos mínimos
   - Estado activo/inactivo

3. **Gestión de Transacciones**
   - Suscripción a fondos con validaciones de negocio
   - Cancelación de suscripciones
   - Control de balance de usuario
   - Historial de transacciones

4. **Sistema de Notificaciones**
   - Email via AWS SES
   - SMS via AWS SNS
   - Preferencias de notificación por usuario
   - Implementación fake para desarrollo

5. **Panel Administrativo**
   - Visualización de usuarios
   - Gestión de fondos
   - Historial de transacciones

## **Arquitectura**

El proyecto implementa Clean Architecture con separación en 5 capas:

```
┌─────────────────────────────────────────────────┐
│              BTGPactual.API (Web API)           │
│                                                 │
│  • Controllers                                  │
│  • Middlewares                                  │
│  • Filters                                      │
│  • Program.cs (Entry Point)                     │
└────────────┬────────────────────────────────────┘
             │
             │ Depends on
             ▼
┌─────────────────────────────────────────────────┐
│         BTGPactual.Application                  │
│                                                 │
│  • Services (Business Logic)                    │
│  • DTOs (Data Transfer)                         │
│  • Validators (FluentValidation)                │
│  • Mappings (AutoMapper)                        │
└────────────┬────────────────────────────────────┘
             │
             │ Depends on
             ▼
┌─────────────────────────────────────────────────┐
│            BTGPactual.Domain                    │
│                                                 │
│  • Entities (Business Objects)                  │
│  • Enums (Business Constants)                   │
│  • Interfaces (Contracts)                       │
│  • Domain Exceptions                            │
└────────────┬────────────────────────────────────┘
             ▲
             │ Implements
             │
┌────────────┴────────────────────────────────────┐
│         BTGPactual.Infrastructure               │
│                                                 │
│  • Repositories (Data Access)                   │
│  • MongoDB Context                              │
│  • External Services (AWS)                      │
│  • Identity (JWT, BCrypt)                       │
└─────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────┐
│            BTGPactual.Shared                    │
│                                                 │
│  • Constants                                    │
│  • Shared Models                                │
│  • Extensions                                   │
│  (Referenced by all layers)                     │
└─────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────┐
│            BTGPactual.Tests                     │
│                                                 │
│  • Unit Tests (145 tests)                       │
│  • Integration Tests                            │
│  (References all projects)                      │
└─────────────────────────────────────────────────┘
```

## **Despliegue en AWS con CloudFormation**

### Prerequisitos

- Cuenta AWS con permisos para crear recursos (EC2, DocumentDB, SES, IAM, VPC).
- Par de llaves EC2 creado en la región deseada.
- Email verificado en AWS SES para notificaciones.

### Crear la pila en CloudFormation

1. Entra a AWS CloudFormation → Create stack → With new resources (standard).

2. Selecciona Upload a template file (YAML) y sube el YAML provisto.

3. Asigna un Stack name.

4. Completa los parámetros obligatorios:
   - KeyPairName: Nombre del par de llaves EC2.
   - SESFromEmail: Email verificado en AWS SES para notificaciones.

5. Create stack y espera a que se complete.

### Conectarse a la instancia EC2

Usa tu clave privada (.pem) y la IP pública de la EC2:

```bash
ssh -i btgpactual-key.pem ec2-user@<EC2_PUBLIC_IP>
```

Una vez conectado, hay que esperar que el SETUP finalice, puede tardar varios minutos.

```bash
# Deberías ver la información de configuración.
cat ~/SETUP-INFO.txt
```

Si ves el archivo SETUP-INFO.txt, el setup ha finalizado correctamente y puedes proceder a desplegar la aplicación.

```bash
# Clonar el repositorio
cd ~/app
git clone https://github.com/tu-usuario/BTGPactual.git
cp ~/app/global-bundle.pem ~/app/BTGPactual/

# Construir la imagen Docker
cd ~/app/BTGPactual
docker build -t btgpactual-api:latest .

# Iniciar el contenedor con Docker Compose
cd ~/app
docker-compose up -d

# Ver logs del contenedor
docker-compose logs -f
```

## **Notas adicionales**

- AWS SNS: Para enviar correos o SMS reales, configurar una cuenta AWS con las credenciales (AWS Access Key, Secret Key y región) en AwsSettings. El proyecto ya integra los servicios de Amazon SES y SNS; solo es necesario descomentar/usar las líneas correspondientes en la configuración de servicios si `UseRealAwsServices=true`. Tenga en cuenta que AWS SNS tiene una cuota predeterminada de gasto de $1.00 USD por cuenta para SMS; en un entorno de prueba, se emplean logs falsos para evitar bloqueos por esta cuota.
- AWS SES: En el entorno sandbox de SES (predeterminado para nuevas cuentas), solo se pueden enviar emails a direcciones o dominios verificados. Las identidades (“From”) también deben estar verificadas para usar SES. Si quieres enviar correos a cualquier destinatario (no solo a los verificados), necesitas solicitar acceso de producción a SES. En tu `.env` puedes usar `UseRealAwsServices=true` para que el servicio use SES real, pero debes asegurarte de que las direcciones que envías son verificadas si estás aún en sandbox.
- Estructura: La solución está organizada en múltiples proyectos: `BTGPactual.API` (punto de entrada Web API), `BTGPactual.Application` (lógica de negocio y DTOs), `BTGPactual.Domain` (entidades y repositorios), `BTGPactual.Infrastructure` (acceso a datos, servicios externos, configuración) y `BTGPactual.Shared` (modelos y constantes comunes).
- Swagger: Se incluye documentación interactiva (Swagger) accesible en /swagger tras iniciar la API para explorar los endpoints disponibles.