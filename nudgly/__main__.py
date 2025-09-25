import os
import sys

from PySide6.QtGui import QGuiApplication
from PySide6.QtQml import QQmlApplicationEngine
from sqlalchemy.types import String

from nudgly.models.conversation_model import ConversationModel
from nudgly.services.config_service import ConfigService

ConfigService.init()

ConfigService.create_section(
    "LLM",
    {
        "api_key": String,
    },
    {
        "api_key": os.getenv("GEMINI_API_KEY", "")
    }
)

from nudgly.services.window_privacy_service import WindowPrivacyService
from nudgly.viewmodels.main_viewmodel import MainViewModel

if __name__ == "__main__":
    app = QGuiApplication(sys.argv)
    engine = QQmlApplicationEngine()

    main_view_model = MainViewModel(
        ConversationModel()
    )
    engine.rootContext().setContextProperty("mainViewModel", main_view_model)

    engine.load("nudgly/views/main.qml")
    engine.load("nudgly/views/answer.qml")

    if not engine.rootObjects():
        sys.exit(-1)

    WindowPrivacyService.enable(engine)

    sys.exit(app.exec())
