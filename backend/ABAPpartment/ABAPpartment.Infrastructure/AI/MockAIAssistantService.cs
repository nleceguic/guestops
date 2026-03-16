using ABAPpartment.Application.Interfaces;

namespace ABAPpartment.Infrastructure.AI;

public class MockAIAssistantService : IAIAssistantService
{
    private static readonly Random _rng = new();

    private const decimal ConfidenceThreshold = 70m;

    public Task<AIAssistantResponse?> GetResponseAsync(
        string guestMessage,
        string guestLanguage,
        AIAssistantContext ctx,
        CancellationToken ct = default)
    {
        var msg = guestMessage.ToLowerInvariant();
        var lang = guestLanguage.ToLowerInvariant();
        var name = ctx.GuestFirstName;

        var topic = DetectTopic(msg);
        var confidence = GetConfidence(topic);

        if (confidence < ConfidenceThreshold)
            return Task.FromResult<AIAssistantResponse?>(null);

        var reply = GenerateReply(topic, msg, ctx, lang, name);
        var isIncident = topic == "incident";

        return Task.FromResult<AIAssistantResponse?>(new AIAssistantResponse(
            Reply: reply,
            Confidence: confidence,
            DetectedTopic: topic,
            ShouldCreateIncident: isIncident
        ));
    }

    private static string DetectTopic(string msg)
    {
        if (ContainsAny(msg, "check-in", "checkin", "llegada", "entrada", "llave", "key",
                             "cerradura", "código", "acceso", "hora de entrada", "check in"))
            return "checkin";

        if (ContainsAny(msg, "check-out", "checkout", "salida", "hora de salida",
                             "dejar", "cuando salir", "check out"))
            return "checkout";

        if (ContainsAny(msg, "wifi", "contraseña wifi", "internet", "red", "password wifi",
                             "wi-fi", "conexión", "clave wifi"))
            return "wifi";

        if (ContainsAny(msg, "metro", "bus", "transporte", "taxi", "uber", "aeropuerto",
                             "cómo llegar", "como llegar", "dirección", "mapa"))
            return "transport";

        if (ContainsAny(msg, "restaurante", "comer", "cenar", "bar", "cafetería",
                             "recomendación", "donde comer", "tapas", "gastronomía"))
            return "food";

        if (ContainsAny(msg, "avería", "roto", "no funciona", "problema", "fuga",
                             "agua", "calefacción", "luz", "electricidad", "mantenimiento",
                             "broken", "doesn't work", "leak", "heating"))
            return "incident";

        if (ContainsAny(msg, "cancelar", "cancelación", "reembolso", "política",
                             "normas", "reglas", "mascotas", "fumar", "fiesta", "ruido"))
            return "rules";

        return "unknown";
    }

    private static decimal GetConfidence(string topic) => topic switch
    {
        "checkin" => 85m + _rng.Next(0, 10),
        "checkout" => 85m + _rng.Next(0, 10),
        "wifi" => 95m + _rng.Next(0, 5),
        "transport" => 80m + _rng.Next(0, 10),
        "food" => 75m + _rng.Next(0, 15),
        "incident" => 90m + _rng.Next(0, 8),
        "rules" => 78m + _rng.Next(0, 12),
        "unknown" => 30m + _rng.Next(0, 20),
        _ => 50m
    };

    private static string GenerateReply(
        string topic, string msg,
        AIAssistantContext ctx,
        string lang, string name)
    {
        var isEs = lang.StartsWith("es");
        var greeting = isEs ? $"Hola {name}," : $"Hi {name},";

        return topic switch
        {
            "checkin" => isEs
                ? $"{greeting} el check-in en {ctx.ApartmentName} es a partir de las 15:00h. " +
                  $"El apartamento está en {ctx.ApartmentAddress}. " +
                  (ctx.SmartLockCode != null
                      ? $"Accederás con cerradura inteligente — el código es {ctx.SmartLockCode}. No necesitas pasar por oficina. "
                      : "Pasarás por nuestra oficina a recoger las llaves. ") +
                  $"Tu reserva es del {ctx.CheckInDate:dd/MM/yyyy} al {ctx.CheckOutDate:dd/MM/yyyy}. ¡Bienvenido!"
                : $"{greeting} check-in at {ctx.ApartmentName} is from 3:00 PM. " +
                  $"The apartment is at {ctx.ApartmentAddress}. " +
                  (ctx.SmartLockCode != null
                      ? $"You'll use a smart lock — the code is {ctx.SmartLockCode}. No need to come to the office. "
                      : "Please come to our office to pick up the keys. ") +
                  $"Your stay is from {ctx.CheckInDate:MM/dd/yyyy} to {ctx.CheckOutDate:MM/dd/yyyy}. Welcome!",

            "checkout" => isEs
                ? $"{greeting} el check-out es antes de las 11:00h el día {ctx.CheckOutDate:dd/MM/yyyy}. " +
                  "Por favor deja las llaves dentro del apartamento y cierra la puerta al salir. " +
                  "Si necesitas salida tardía, contáctanos con 24h de antelación."
                : $"{greeting} check-out is before 11:00 AM on {ctx.CheckOutDate:MM/dd/yyyy}. " +
                  "Please leave the keys inside the apartment and close the door when you leave. " +
                  "If you need a late check-out, contact us 24h in advance.",

            "wifi" => isEs
                ? $"{greeting} la red WiFi del apartamento es **AB_Guest_{ctx.ApartmentName.Replace(" ", "")}** " +
                  "y la contraseña está en el manual de bienvenida sobre la mesa del salón. " +
                  "Si tienes problemas de conexión, reinicia el router (caja negra en el salón). ¿Necesitas algo más?"
                : $"{greeting} the WiFi network is **AB_Guest_{ctx.ApartmentName.Replace(" ", "")}** " +
                  "and the password is in the welcome guide on the living room table. " +
                  "If you have connection issues, restart the router (black box in the living room). Anything else?",

            "transport" => isEs
                ? $"{greeting} el apartamento en {ctx.ApartmentDistrict} tiene excelentes conexiones. " +
                  "Metro más cercano: L4 (línea amarilla) a 5 min a pie. " +
                  "Para el aeropuerto: Aerobús desde Pl. Catalunya (40 min, 6.75€) o taxi (~35€). " +
                  "También puedes usar Glovo para comida a domicilio o Cabify para taxis."
                : $"{greeting} the apartment in {ctx.ApartmentDistrict} has great connections. " +
                  "Nearest metro: L4 (yellow line) 5 min walk. " +
                  "For the airport: Aerobús from Pl. Catalunya (40 min, €6.75) or taxi (~€35).",

            "food" => isEs
                ? $"{greeting} el barrio de {ctx.ApartmentDistrict} tiene opciones excelentes. " +
                  "Para tapas: prueba los bares de la zona alta del barrio. " +
                  "Para desayuno: cualquier cafetería de la calle principal. " +
                  "Para cena romántica: busca en Google Maps 'restaurantes {ctx.ApartmentDistrict} Barcelona'. " +
                  "¡Buen provecho!"
                : $"{greeting} {ctx.ApartmentDistrict} has great options! " +
                  "For tapas: try the bars in the upper part of the neighborhood. " +
                  "For dinner: search Google Maps for 'restaurants {ctx.ApartmentDistrict} Barcelona'. Enjoy!",

            "incident" => isEs
                ? $"{greeting} lamentamos los inconvenientes. Hemos registrado tu incidencia y nuestro equipo de mantenimiento " +
                  "se pondrá en contacto contigo en menos de 2 horas. " +
                  "Si es urgente, llámanos al +34 93 XXX XX XX (disponible 24/7)."
                : $"{greeting} we're sorry for the inconvenience. We've logged your issue and our maintenance team " +
                  "will contact you within 2 hours. " +
                  "For emergencies, call us at +34 93 XXX XX XX (24/7).",

            "rules" => isEs
                ? $"{greeting} en {ctx.ApartmentName} está prohibido fumar, organizar fiestas y se admiten mascotas pequeñas previa consulta. " +
                  "El ruido debe mantenerse bajo control especialmente entre 22:00 y 9:00h. " +
                  "Para cancelaciones con más de 48h de antelación se aplica reembolso del 80%. " +
                  "¿Tienes alguna duda específica?"
                : $"{greeting} at {ctx.ApartmentName} smoking and parties are not allowed. Small pets are accepted upon request. " +
                  "Noise must be kept down between 10 PM and 9 AM. " +
                  "Cancellations more than 48h in advance receive an 80% refund.",

            _ => isEs
                ? $"{greeting} gracias por tu mensaje. Un miembro de nuestro equipo te responderá en breve."
                : $"{greeting} thank you for your message. A member of our team will get back to you shortly."
        };
    }

    private static bool ContainsAny(string text, params string[] keywords)
        => keywords.Any(text.Contains);
}