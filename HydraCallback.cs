using Hydra;
using MachineCheck.Services;
using System;
using System.Drawing;

[assembly: CallbackAssemblyDescription("Przegląd maszyny",
"Przegląd maszyny",
"Krzysztof Kurowski",
"1.0",
"8.0",
"05-08-2025")]

namespace MachineCheck
{
    public class HydraCallback
    {
        [SubscribeProcedure((Procedures)Procedures.POBEdycja, "callback na zasobie")]
        public class MainCallback : Callback
        {
            private ClaWindow Button;
            private ClaWindow ButtonParent;
            private readonly string _connectionString = "user id=xxxx;password=xxxx;Data Source=xxxx;Trusted_Connection=no;database=" + Runtime.ActiveRuntime.Repository.Connection.Database + ";connection timeout=5;";
            private readonly DatabaseService _dbService;

            public MainCallback()
            {
                _dbService = new DatabaseService(_connectionString);
            }

            public override void Init()
            {
                AddSubscription(true, 0, Events.OpenWindow, new TakeEventDelegate(OnOpenWindow));
                AddSubscription(false, 0, Events.ResizeWindow, new TakeEventDelegate(ChangeWindow));
            }

            public bool OnOpenWindow(Procedures ProcId, int ControlId, Events Event)
            {
                ClaWindow Parent = GetWindow();

                ButtonParent = Parent.AllChildren["?POB:RejestrowacUzycie"]; // od ktorego przycisku
                Button = Parent.Children["?Tab1"].Children.Add(ControlTypes.button); // w ktorej belce
                Button.Visible = true;

                Button.TextRaw = "Przegląd maszyny";

                Button.Bounds = new Rectangle(Convert.ToInt32(ButtonParent.XposRaw), Convert.ToInt32(ButtonParent.YposRaw) + 27, 80, 20);

                AddSubscription(false, Button.Id, Events.Accepted, new TakeEventDelegate(Zdarzenie));

                return true;
            }

            public bool ChangeWindow(Procedures ProcId, int ControlId, Events Event)
            {
                Button.Bounds = new Rectangle(Convert.ToInt32(ButtonParent.XposRaw), Convert.ToInt32(ButtonParent.YposRaw) + 27, 80, 20);
                return true;
            }

            public bool Zdarzenie(Procedures ProcId, int ControlId, Events Event)
            {
                var form = new MachineCheckForm(ProdObiekty.POB_Id, Hydra.Runtime.Config.NumerOperatora, _dbService);
                form.Show();

                return true;
            }

            public override void Cleanup()
            {
            }
        }
    }
}