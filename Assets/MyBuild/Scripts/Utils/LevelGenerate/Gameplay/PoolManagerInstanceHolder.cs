using MyBuild.Scripts.Utils.LevelGenerate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBuild.Scripts.Utils.LevelGenerate
{
    /// <summary>
    /// Временный холдер, через который DI получает ссылку на загруженный PoolManager.
    /// </summary>
    public class PoolManagerInstanceHolder
    {
        public PoolManager Instance { get; set; }
    }
}
