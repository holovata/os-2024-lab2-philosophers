using System;

namespace philosophers_try2
{
    public class Fork
    {
        public int Name { get; }
        private bool _isBeingUsed;
        private Philosopher _beingUsedBy;

        public bool IsBeingUsed
        {
            get => _isBeingUsed;
            set
            {
                if (value == _isBeingUsed)
                {
                    var message = value
                        ? $"Не можна призначити виделку {Name} (вона вже зайнята)"
                        : $"Не можна звільнити виделку {Name} (вона вже вільна)";
                    throw new Exception(message);
                }
                _isBeingUsed = value;
            }
        }

        public Philosopher BeingUsedBy
        {
            get => _beingUsedBy;
            set
            {
                if (value == null && _beingUsedBy == null || value != null && _beingUsedBy != null)
                {
                    var message = value == null
                        ? $"Не можна призначити виделку {Name} жодному філософу (вона вже вільна)"
                        : $"Не можна призначити виделку {Name} філософу Ph{value} (вона вже призначена філософу Ph{_beingUsedBy})";
                    throw new Exception(message);
                }
                _beingUsedBy = value;
            }
        }

        public Fork(int name)
        {
            Name = name;
            _isBeingUsed = false;
            _beingUsedBy = null;
        }

        public void PickUp(Philosopher philosopher)
        {
            if (!_isBeingUsed)
            {
                _isBeingUsed = true;
                _beingUsedBy = philosopher;
            }
            else
                throw new InvalidOperationException($"Виделка {Name} вже використовується.");
        }

        public void PutDown()
        {
            _isBeingUsed = false;
            _beingUsedBy = null;
        }
    }
}
