using System;

public readonly struct LayerInfo : IEquatable<LayerInfo> {
    public readonly string layerName;
    public readonly int layer;
    public LayerInfo(string layerName, int layer) {
        this.layerName = layerName;
        this.layer = layer;
    }

    public bool Equals(LayerInfo other) {
        return layerName == other.layerName || layer == other.layer;
    }

    public override string ToString() {
        return $"(\"{layerName}\", {layer})";
    }
}
