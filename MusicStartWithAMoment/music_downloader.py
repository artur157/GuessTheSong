import os
import time
import logging
import requests
from bs4 import BeautifulSoup
import re
import sys
import shutil
import multiprocessing as mp

TIMEOUT = 2                 # интервал обращения за страницами
MAX_RETRY = 3               # макс. кол-во попыток получить страницу
DOMAIN = 'https://ru.apporange.space'
DIRECTORY = 'F:'            # директория ехе
total = 0


def get_response(url):     # получить страницу по url
    headers = {
        'User-Agent': 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.181 Safari/537.36',
        'Pragma': 'no-cache'
    }

    for i in range(MAX_RETRY):
        try:
            return requests.get(url, headers=headers, timeout=TIMEOUT)
        except Exception as ex:
            print("Cannot crawl url {} by reason: {}. Retry in 1 sec".format(url, ex))
            time.sleep(1)
    return requests.Response()


def get_clear_title(title):    # из названия песни убираем всё лишнее, модифицируем в унифицированный вид   НЕ ИСПОЛЬЗУЕТСЯ
    # обрезаем вступление "скачать песню..."
    clear_title = title[title.find('-') + 2:]
    ct = clear_title.split('_')
    ct2 = ct[:len(ct)-1]
    clear_title = ' '.join(ct2)

    # убираем пробелы в начале и конце
    clear_title = clear_title.strip()

    # заменить ё на е
    clear_title = clear_title.replace('ё', 'е')

    # все слова после пробела - с большой буквы если англ. Иначе нет
    words = clear_title.split(' ')
    new_words = []

    if (re.search(r'[a-zA-Z0-9]', clear_title[0])):
        for word in words:
            new_words.append(word.title())
    else:
        for word in words:
            new_words.append(word.lower())
        new_words[0] = new_words[0].title()

    clear_title = ' '.join(new_words)

    # слеши заменяем на -
    clear_title = clear_title.replace('/', ' - ')

    return clear_title


def savee(path, href):
    music_file = get_response(href).content
    with open(path, 'wb') as file:
        file.write(music_file)


def parse(html):     # на входе строка url
    global total, artist_name

    try:
        soup = BeautifulSoup(html, 'lxml')
        links = soup.select('a.track__download-btn')
        titles = soup.select('.track__title')

        # хотим проверять длительности песен
        musicset = soup.select('.track__fulltime')
        minutes = []
        for i in range(0, min(len(musicset), len(links))):
            t = musicset[i].text
            minutes.append(int(t[:t.find(':')]))

        # создаем папку если ее нет
        folder_path = 'data\\' + artist_name
        if not os.path.exists(folder_path):  # Если пути не существует создаем его
            os.makedirs(folder_path)

        dataset = []

        for i in range(0, len(links)):
            if minutes[i] > 0 and minutes[i] < 8:  # чтобы песня не очень долгая была
                link = links[i]
                # music_file = get_response(link.get('href')).content
                title = titles[i].text.strip()
                title = title.replace('/', ' - ')
                title = title.replace('?', '')
                title = title.replace(': ', ' - ')
                title = title.replace('"', '')
                # print(title + ' ' + str(minutes[i]))
                if not os.path.exists(folder_path + '\\' + title + '.mp3'):
                    path = folder_path + '\\' + title + '.mp3'
                    dataset.append((path, link.get('href')))
                    total += 1
                    # print(total)

        with mp.Pool(mp.cpu_count()) as pool:
            pool.starmap(savee, dataset)

    except Exception as ex:
        print('Exception while parsing 2')
        print(str(ex))


def get_artist_url(name):
    global artist_name

    url = DOMAIN + '/search?q=' + '+'.join(name.split(' '))

    html = get_response(url).text

    try:
        soup = BeautifulSoup(html, 'lxml')
        links = soup.select('.album-link')
        artists = soup.select('.album-title')
        artist_name = artists[0].text

        # теперь картинки
        spans = soup.select('.album-image')
        st = spans[0].get('style')
        st = st[st.find('url') + 5:]
        image_url = 'https:' + st[:len(st) - 2]
        image = get_response(image_url).content
        with open('data\\' + artist_name + '.jpg', 'wb') as file:
           file.write(image)

        return links[0].get('href')
    except Exception as ex:
        print('Exception while parsing')
        print(str(ex))

    return None


if __name__ == '__main__':
    spisok = sys.argv[1:]
    artist_name = ' '.join(spisok)
    #artist_name = 'Rolling Stones'

    headers = {
        'User-Agent': 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.181 Safari/537.36',
        'Pragma': 'no-cache'
    }
    logging.basicConfig(level=logging.ERROR)

    url = DOMAIN + get_artist_url(artist_name)

    if url != None:
        for i in range(0, 3):    #  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            html = get_response(url + '/start/' + str(48*i)).text
            parse(html)

    if total < 14:   # если композиций меньше 14, то для теста не хватит. Удаляем папку
        try:
            shutil.rmtree(DIRECTORY + '\\GuessTheSong\\' + artist_name)
            os.remove(DIRECTORY + '\\GuessTheSong\\' + artist_name + '.jpg')
            print(1)
        except Exception as ex:
            print('Exception while removing')
            print(str(ex))
    else:
        print(0)
