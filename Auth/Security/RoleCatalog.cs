namespace Auth.Security;

public static class RoleCatalog
{
    public const string Admin = "admin";
    public const string Veterinarian = "medico_veterinario";
    public const string Handler = "manejador_animal";
    public const string Applicator = "aplicador";
    public const string Intern = "estagiario_medvet";

    public static readonly IReadOnlyDictionary<string, string> Descriptions = new Dictionary<string, string>
    {
        [Admin] = "Administrador: gerencia usuários e suas roles no provedor de autenticação.",
        [Veterinarian] = "Médico veterinário.",
        [Handler] = "Responsável pelo manejo dos animais.",
        [Applicator] = "Responsável pela aplicação de vacinas.",
        [Intern] = "Estagiário de medicina veterinária: incorpora aplicador e manejador_animal."
    };

    public static string[] Expand(IEnumerable<string> roles)
    {
        var result = roles.ToHashSet(StringComparer.Ordinal);
        if (result.Contains(Intern)) { result.Add(Applicator); result.Add(Handler); }
        return result.Order(StringComparer.Ordinal).ToArray();
    }
}
