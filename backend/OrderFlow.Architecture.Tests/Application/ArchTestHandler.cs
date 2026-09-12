using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ArchitectureTests
{
    public class ArchTestHandler
    {
        private static readonly Architecture Architecture = new ArchLoader().LoadAssemblies(
            System.Reflection.Assembly.Load("OrderFlow.Application")
        ).Build();

        [Fact]
        public void HandlersShouldResideInCommandNamespace()
        {
            var portNamespacePatternCommand = "OrderFlow.Application.*.Command";

            Classes()
            .That()
            .ResideInNamespace(portNamespacePatternCommand, true)
            .Should()
            .HaveNameEndingWith("Handler")
            .OrShould()
            .HaveNameEndingWith("Command")
            .OrShould()
            .HaveNameEndingWith("Factory")
            .Because("Los manejadores deben estar en la capa de aplicaci�n y deben tener nombres que terminen con 'Handler'")
            .Check(Architecture);
        }

        [Fact]
        public void HandlersShouldResideInQueryNamespace()
        {
            var portNamespacePatternQuery = "OrderFlow.Application.*.Query";

            Classes()
            .That()
            .ResideInNamespace(portNamespacePatternQuery, true)
            .Should()
            .HaveNameEndingWith("Handler")
            .OrShould()
            .HaveNameEndingWith("Query")
            .OrShould()
            .HaveNameEndingWith("Dto")
            .Because("Los manejadores de consulta deben estar en la capa de aplicaci�n y deben tener nombres que terminen con 'Handler', 'Query' o 'Dto'")
            .Check(Architecture);
        }
    }
}
