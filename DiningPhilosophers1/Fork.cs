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
                        ? $"Не можна призначити виделку F{Name} (вона вже зайнята)"
                        : $"Не можна звільнити виделку F{Name} (вона вже вільна)";
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
                        ? $"Не можна призначити виделку F{Name} жодному філософу (вона вже вільна)"
                        : $"Не можна призначити виделку F{Name} філософу Ph{value} (вона вже призначена філософу Ph{_beingUsedBy})";
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
		
	}
}