using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlaglinesAndSuch
{
    [CustomEntity("FlaglinesAndSuch/FlagLightFade")]
    class FlagLightFade : Trigger
    {
		public float LightAddFrom;

		public float LightAddTo;

		public PositionModes PositionMode;

		String flag;
		bool flagState;
		bool resetFlag;

		bool wasSuccesfullyTriggered;

		public FlagLightFade(EntityData data, Vector2 offset) : base(data, offset)
		{
			AddTag(Tags.TransitionUpdate);
			LightAddFrom = data.Float("LightAddFrom");
			LightAddTo = data.Float("LightAddTo");
			PositionMode = data.Enum("positionMode", PositionModes.NoEffect);

			flag = data.Attr("Flag");
			flagState = data.Bool("Flag_state");
			resetFlag = data.Bool("Reset_Flag");
		}

        public override void OnEnter(Player player)
        {
			//Console.WriteLine(PositionMode + flag + resetFlag + LightAddFrom + LightAddTo + wasSuccesfullyTriggered + "Flaglines Light dump: Entry");
			base.OnEnter(player);
			if ((Scene as Level).Session.GetFlag(flag) == flagState)
			{
				wasSuccesfullyTriggered = true;
			}
		}
        public override void OnStay(Player player)
		{
			//Console.WriteLine(PositionMode + flag + resetFlag + LightAddFrom + LightAddTo + wasSuccesfullyTriggered + "Flaglines Light dump: Stay");
			if (wasSuccesfullyTriggered)
			{
				Level level = base.Scene as Level;
				float num = level.Session.LightingAlphaAdd = LightAddFrom + (LightAddTo - LightAddFrom) * MathHelper.Clamp(GetPositionLerp(player, PositionMode), 0f, 1f);
				level.Lighting.Alpha = level.BaseLightingAlpha + num;

			}

		}
        public override void OnLeave(Player player)
        {
			//Console.WriteLine(PositionMode + flag + resetFlag + LightAddFrom + LightAddTo + wasSuccesfullyTriggered + "Flaglines Light dump: Exit");
			base.OnLeave(player);
			if (resetFlag & wasSuccesfullyTriggered)
			{
				(Scene as Level).Session.SetFlag(flag, !flagState);

			}
			wasSuccesfullyTriggered = false;
		}
	}
}
