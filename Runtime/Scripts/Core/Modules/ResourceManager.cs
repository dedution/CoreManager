using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;

namespace core.modules
{
    public class ResourceManager : BaseModule
    {
        // TODO:
        // This module will handle the load and unload of resources
        // This module will also handle the way other modules and game logic creates pools of data
        // This module will work on a queue based system
        // Everything related to writing and reading of files and data will be done through this module

        public override void onInitialize()
        {

        }

        public override void UpdateModule(float deltaTime, float unscaledDeltaTime)
        {

        }
    }
}