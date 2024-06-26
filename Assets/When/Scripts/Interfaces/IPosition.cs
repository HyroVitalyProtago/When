﻿using UnityEngine;

namespace When.Interfaces {
    public interface IPosition {
        Vector3 Position { get; }
        Vector3 DeltaPosition { get; }
    }
}
