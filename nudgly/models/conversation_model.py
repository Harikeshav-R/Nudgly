from typing import TypedDict, List, Union, Literal


class ImageUrl(TypedDict):
    url: str


class TextPart(TypedDict):
    type: Literal["text"]
    text: str


class ImageUrlPart(TypedDict):
    type: Literal["image_url"]
    image_url: ImageUrl


ContentPart = Union[TextPart, ImageUrlPart]


class Message(TypedDict):
    role: str
    content: List[ContentPart]


class Conversation(TypedDict):
    model: str
    messages: List[Message]
