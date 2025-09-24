import logging
from logging.handlers import RotatingFileHandler

from nudgly.constants import Constants


class Logger:
    _logger = None
    _log_file = Constants.LOG_FILE_PATH
    _max_bytes = 10 * 1024 * 1024  # 10 MB
    _backup_count = 5
    _log_level = logging.DEBUG
    _formatter = logging.Formatter(
        "%(asctime)s [%(levelname)s] %(module)s.%(funcName)s: %(message)s",
        datefmt="%Y-%m-%d %H:%M:%S"
    )

    @classmethod
    def _initialize(cls):
        if cls._logger is not None:
            return

        # Main package logger
        cls._logger = logging.getLogger("PackageLogger")
        cls._logger.setLevel(cls._log_level)
        cls._logger.propagate = False

        # Ensure log directory exists
        cls._log_file.parent.mkdir(parents=True, exist_ok=True)

        # File handler (rotating)
        file_handler = RotatingFileHandler(
            filename=cls._log_file,
            maxBytes=cls._max_bytes,
            backupCount=cls._backup_count,
            encoding="utf-8"
        )
        file_handler.setFormatter(cls._formatter)

        # Console handler
        console_handler = logging.StreamHandler()
        console_handler.setFormatter(cls._formatter)

        # Attach handlers to our package logger
        cls._logger.addHandler(file_handler)
        cls._logger.addHandler(console_handler)

        # Also attach handlers to the root logger for external libraries
        root_logger = logging.getLogger()
        root_logger.setLevel(cls._log_level)
        root_logger.addHandler(file_handler)
        root_logger.addHandler(console_handler)

    @classmethod
    def get_logger(cls):
        cls._initialize()
        return cls._logger

    @classmethod
    def _log(cls, level, msg, *args, **kwargs):
        # stacklevel=3 ensures caller info points to the original caller
        cls.get_logger().log(level, msg, *args, stacklevel=3, **kwargs)

    @classmethod
    def debug(cls, msg, *args, **kwargs):
        cls._log(logging.DEBUG, msg, *args, **kwargs)

    @classmethod
    def info(cls, msg, *args, **kwargs):
        cls._log(logging.INFO, msg, *args, **kwargs)

    @classmethod
    def warning(cls, msg, *args, **kwargs):
        cls._log(logging.WARNING, msg, *args, **kwargs)

    @classmethod
    def error(cls, msg, *args, **kwargs):
        cls._log(logging.ERROR, msg, *args, **kwargs)

    @classmethod
    def critical(cls, msg, *args, **kwargs):
        cls._log(logging.CRITICAL, msg, *args, **kwargs)
