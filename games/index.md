---
layout: page
title: Game Development Notes
permalink: /games/
---

게임 개발 과정에서의 노트와 경험을 기록합니다.

{% assign game_posts = site.posts | where_exp: "post", "post.categories contains 'games'" %}
{% if game_posts.size > 0 %}
<ul>
  {% for post in game_posts %}
  <li><a href="{{ post.url | relative_url }}">{{ post.title }}</a> — {{ post.date | date: "%Y-%m-%d" }}</li>
  {% endfor %}
</ul>
{% else %}
<p><em>아직 게시된 글이 없습니다.</em></p>
{% endif %}