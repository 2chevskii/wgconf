using System.Net.Sockets;
using System.Reflection;
using System.Linq;

namespace WgConf.Tests;

public class AddressFamilyExtensionsTests
{
    [Fact]
    public void FriendlyName_And_MaxPrefixLength_ReturnExpectedValues()
    {
        Assert.Equal("IPv4", InvokeFriendlyName(AddressFamily.InterNetwork));
        Assert.Equal("IPv6", InvokeFriendlyName(AddressFamily.InterNetworkV6));
        Assert.Equal(32, InvokeMaxPrefixLength(AddressFamily.InterNetwork));
        Assert.Equal(128, InvokeMaxPrefixLength(AddressFamily.InterNetworkV6));
    }

    [Fact]
    public void FriendlyName_Unsupported_Throws()
    {
        var ex = Assert.Throws<TargetInvocationException>(() =>
            InvokeFriendlyName((AddressFamily)999)
        );
        Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);
    }

    [Fact]
    public void MaxPrefixLength_Unsupported_Throws()
    {
        var ex = Assert.Throws<TargetInvocationException>(() =>
            InvokeMaxPrefixLength((AddressFamily)999)
        );
        Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);
    }

    private static string InvokeFriendlyName(AddressFamily family)
    {
        var method = GetExtensionMethod("FriendlyName");
        return (string)method.Invoke(null, new object[] { family })!;
    }

    private static int InvokeMaxPrefixLength(AddressFamily family)
    {
        var method = GetExtensionMethod("MaxPrefixLength");
        return (int)method.Invoke(null, new object[] { family })!;
    }

    private static MethodInfo GetExtensionMethod(string nameFragment)
    {
        var type = typeof(CIDR).Assembly.GetType("WgConf.Extensions.AddressFamilyExtensions");
        Assert.NotNull(type);

        var method = type!
            .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Single(m =>
                (m.Name.Equals(nameFragment, StringComparison.Ordinal)
                    || m.Name.Equals($"get_{nameFragment}", StringComparison.Ordinal))
                && m.GetParameters().Length == 1
            );

        return method;
    }
}
