using System;

namespace FullSerializer.Internal {
    public class patch_fsEnumConverter : fsEnumConverter {
        // This handles non-vanilla enum values that get handled as ints instead of strings when saving.
        public extern fsResult orig_TrySerialize(object instance, out fsData serialized, Type storageType);
        public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType) {
            if (!Serializer.Config.SerializeEnumsAsInteger) {
                if (Enum.GetName(storageType, instance) == null) {
                    //Log.Debug($"{instance} is not a valid enum value for {storageType}!");
                    serialized = new fsData(Convert.ToInt64(instance));
                    //Log.Debug($"We have converted it to an int {serialized}");
                    return fsResult.Success;
                }
                else {
                    return orig_TrySerialize(instance, out serialized, storageType);
                }
            }
            else {
                return orig_TrySerialize(instance, out serialized, storageType);
            }
        }
    }
}