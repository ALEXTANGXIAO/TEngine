using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TEngine;
using TEngine.EntityModule;

public class TestUpdateCmpt : EntityComponent,IUpdate
{
    public void Update()
    {
        TLogger.LogInfo("update");
    }
}
