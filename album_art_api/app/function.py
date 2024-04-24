from fastapi import FastAPI, HTTPException
from mangum import Mangum
import ytmusicapi as yt
from aiohttp import ClientSession
from pydantic import BaseModel
from result import Result, Ok, Err


class AlbumArtResponse(BaseModel):
    title: str
    url: str


app = FastAPI()
ytm = yt.YTMusic("oauth.json")


async def get_album_art_ytm(search: str) -> Result[str, None]:
    try:
        search_results = ytm.search(search)
        video_id = search_results[0]["videoId"]

        get_song_results = ytm.get_song(video_id)
        thumbnails = get_song_results["videoDetails"]["thumbnail"]["thumbnails"]
        album_art = thumbnails[-1]["url"].split("?")[0]

        return Ok(str(album_art))
    except:
        return Err(None)


async def get_album_art_itunes(search: str) -> Result[str, None]:
    try:
        async with ClientSession() as session:
            async with session.get("") as resp:
                ...
                return Ok("")
    except:
        return Err(None)


@app.get("/")
async def root() -> dict[str, str]:
    return {"message": "API ready"}


@app.get("/albumarts/{search}")
async def get_album_art(search: str) -> AlbumArtResponse:
    ytm_result = await get_album_art_ytm(search)
    match ytm_result:
        case Ok(s):
            return AlbumArtResponse(title=search, url=ytm_result.unwrap())
        case Err(_):
            raise HTTPException(status_code=404, detail="Album art not found")


handler = Mangum(app, lifespan="off")
