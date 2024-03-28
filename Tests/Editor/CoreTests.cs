using System.Collections;
using System.Collections.Generic;
using core;
using core.modules;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CoreTests
{
    [Test]
    public void BasicCoreManagerTest()
    {
        // Initialize Game Manager
        GameManager.Instance.Init();
        Assert.Fail();
    }
}
