// <auto-generated>
//   This file was generated by a tool; you should avoid making direct changes.
//   Consider using 'partial classes' to extend these types
//   Input: AssetBundleConfigData.proto
// </auto-generated>

#region Designer generated code
#pragma warning disable CS0612, CS0618, CS1591, CS3021, CS8981, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
[global::ProtoBuf.ProtoContract()]
public partial class AssetBundleBase : global::ProtoBuf.IExtensible
{
    private global::ProtoBuf.IExtension __pbn__extensionData;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
        => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    [global::ProtoBuf.ProtoMember(1)]
    public uint Crc { get; set; }

    [global::ProtoBuf.ProtoMember(2)]
    [global::System.ComponentModel.DefaultValue("")]
    public string ABName { get; set; } = "";

    [global::ProtoBuf.ProtoMember(3)]
    [global::System.ComponentModel.DefaultValue("")]
    public string AssetName { get; set; } = "";

    [global::ProtoBuf.ProtoMember(4)]
    [global::System.ComponentModel.DefaultValue("")]
    public string Path { get; set; } = "";

    [global::ProtoBuf.ProtoMember(5)]
    public global::System.Collections.Generic.List<string> ABDependcies { get; } = new global::System.Collections.Generic.List<string>();

}

[global::ProtoBuf.ProtoContract()]
public partial class AssetBundleConfigData : global::ProtoBuf.IExtensible
{
    private global::ProtoBuf.IExtension __pbn__extensionData;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
        => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    [global::ProtoBuf.ProtoMember(1, Name = @"version")]
    [global::System.ComponentModel.DefaultValue("")]
    public string Version { get; set; } = "";

    [global::ProtoBuf.ProtoMember(2)]
    public global::System.Collections.Generic.List<AssetBundleBase> AssetBundleLists { get; } = new global::System.Collections.Generic.List<AssetBundleBase>();

}

#pragma warning restore CS0612, CS0618, CS1591, CS3021, CS8981, IDE0079, IDE1006, RCS1036, RCS1057, RCS1085, RCS1192
#endregion
