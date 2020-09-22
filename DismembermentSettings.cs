using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DismembermentSue
{
    public class DismembermentSettings
    {
		private static DismembermentSettings _instance;

		DismembermentMissionBusiness dismemberment;

		public static DismembermentSettings Instance()
		{
			if(null == _instance)
			{
				_instance = new DismembermentSettings();
			}
			return _instance;
		}


		public DismembermentMissionBusiness GetDismembermentMissionBusiness()
		{
			if(null == dismemberment)
			{
				dismemberment = new DismembermentMissionBusiness();
			}
			return dismemberment;
		}
	}
}
