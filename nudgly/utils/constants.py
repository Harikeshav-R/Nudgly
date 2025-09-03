from pathlib import Path
from platformdirs import user_data_dir, user_log_dir, user_config_dir


class Constants:
    CONFIG_PATH = Path(user_config_dir("Nudgly", "Nudgly"), "config.db").resolve()
    DATABASE_PATH = Path(user_data_dir("Nudgly", "Nudgly"), "nudgly.db").resolve()
    LOG_FILE_PATH = Path(user_log_dir("Nudgly", "Nudgly"), "nudgly.log").resolve()