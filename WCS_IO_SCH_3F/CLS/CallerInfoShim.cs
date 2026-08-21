//  [CallerFilePath] / [CallerMemberName] 는 .NET 4.5 부터 mscorlib 에 들어간다.
//  이 프로젝트는 v4.0 을 대상으로 하고 있어 참조 어셈블리에 그 특성이 없다.
//  특성은 컴파일할 때만 쓰이고 실행할 때는 값이 이미 문자열로 박혀 있으므로,
//  같은 이름으로 여기서 선언해 두면 v4.0 그대로 두고도 쓸 수 있다.
//  (.NET 4.5 이상으로 올리게 되면 이 파일은 지워야 형식이 겹치지 않는다.)
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    public sealed class CallerFilePathAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    public sealed class CallerMemberNameAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    public sealed class CallerLineNumberAttribute : Attribute
    {
    }
}
