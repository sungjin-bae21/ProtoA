//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: project_msg.proto
namespace Message
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"ProjectA_Msg")]
  public partial class ProjectA_Msg : global::ProtoBuf.IExtensible
  {
    public ProjectA_Msg() {}
    
    private byte[] _session_id;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"session_id", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public byte[] session_id
    {
      get { return _session_id; }
      set { _session_id = value; }
    }
    private string _message_type;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"message_type", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public string message_type
    {
      get { return _message_type; }
      set { _message_type = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}