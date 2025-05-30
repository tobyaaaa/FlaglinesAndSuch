using Celeste;
using Celeste.Mod;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlaglinesAndSuch
{
	[CustomEntity("FlaglinesAndSuch/MiniTextboxIfFlag")]
	class MiniTextboxIfFlag : Trigger
	{
		private enum Modes
		{
			OnPlayerEnter,
			OnLevelStart,
			OnTheoEnter
		}

		private EntityID id;

		private string[] dialogOptions;

		private Modes mode;

		private bool triggered;

		private bool onlyOnce;

		private int deathCount;

		String flag;
		bool flagState;
		bool resetFlag;

		public MiniTextboxIfFlag(EntityData data, Vector2 offset, EntityID id)
			: base(data, offset)
		{
			this.id = id;
			mode = data.Enum("mode", Modes.OnPlayerEnter);
			dialogOptions = data.Attr("dialog_id").Split(',');
			onlyOnce = data.Bool("only_once");
			deathCount = data.Int("death_count", -1);
			if (mode == Modes.OnTheoEnter)
			{
				Add(new HoldableCollider((Action<Holdable>)delegate
				{
					Trigger();
				}, (Collider)null));
			}

			flag = data.Attr("Flag");
			flagState = data.Bool("Flag_State");
			resetFlag = data.Bool("Reset_Flag");
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (mode == Modes.OnLevelStart)
			{
				Trigger();
			}
		}

		public override void OnEnter(Player player)
		{
			if (mode == Modes.OnPlayerEnter)
			{
				Trigger();
			}
		}

		private void Trigger()
		{
			if ((Scene as Level).Session.GetFlag(flag) != flagState)
			{
				return;
			}

			if (!triggered && (deathCount < 0 || (base.Scene as Level).Session.DeathsInCurrentLevel == deathCount))
			{
				triggered = true;
				if (resetFlag)
				{
					(Scene as Level).Session.SetFlag(flag, !flagState);
				}

				base.Scene.Add(new MiniTextbox(Calc.Random.Choose(dialogOptions)));
				if (onlyOnce)
				{
					(base.Scene as Level).Session.DoNotLoad.Add(id);
				}
			}


		}
	}
}
