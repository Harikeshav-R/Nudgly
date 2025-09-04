import logging

from typing import Optional

from google.genai import chats
from PySide6.QtCore import (
    QAbstractListModel,
    QModelIndex,
    Qt,
    QObject, QByteArray
)
from PySide6.QtGui import QImage
from PySide6.QtQuick import QQuickImageProvider

logger = logging.getLogger(__name__)


class ConversationModel(QAbstractListModel):
    RoleRole = Qt.UserRole + 1
    TextContentRole = Qt.UserRole + 2
    ImageContentRole = Qt.UserRole + 3

    def __init__(self, chat: chats.Chat, parent: Optional[QObject] = None):
        super().__init__(parent)

        self.chat = chat
        self._history = []

        self._image_cache = {}  # id -> QImage
        self._next_image_id = 0

        logger.debug(f"Initialized ConversationModel.")

    def rowCount(self, parent=QModelIndex()) -> int:
        return len(self._history)

    def data(self, index: QModelIndex, role: int = Qt.DisplayRole):
        if not index.isValid() or not (0 <= index.row() < len(self._history)):
            return None

        item = self._history[index.row()]

        if role == self.RoleRole:
            return item.role

        elif role == self.TextContentRole:
            for part in item.parts:
                if part.text and not part.inline_data:
                    return part.text
            return ""

        elif role == self.ImageContentRole:
            for part in item.parts:
                if part.inline_data and not part.text:
                    ba = QByteArray.fromRawData(part.inline_data.data)
                    image = QImage.fromData(ba, "PNG")

                    image_id = f"img_{self._next_image_id}"
                    self._next_image_id += 1
                    self._image_cache[image_id] = image

                    return image_id

            return ""

        return ""

    def roleNames(self):
        return {
            self.RoleRole: b"role",
            self.TextContentRole: b"textContent",
            self.ImageContentRole: b"imageContent",
        }

    def add_messages(self):
        self.beginResetModel()
        self._history = self.chat.get_history()
        self.endResetModel()

        logger.info(f"Added user and model messages to conversation history")

    def get_image_by_id(self, image_id: str) -> QImage:
        return self._image_cache.get(image_id)


class ConversationImageProvider(QQuickImageProvider):
    def __init__(self, model: ConversationModel):
        super().__init__(QQuickImageProvider.Image)
        self.model = model

    def requestImage(self, id_: str, size, requested_size):
        image = self.model.get_image_by_id(id_)
        if image:
            return image
        return QImage()
