---
layout: page
title: Unity Engineering
permalink: /unity/
---

Unity 엔진과 관련된 엔지니어링 글을 모아둡니다.

{% assign unity_articles = site.pages | where_exp: "p", "p.path contains 'unity/' and p.name != 'index.md'" | sort: "title" %}
{% assign unity_posts = site.posts | where_exp: "post", "post.categories contains 'unity'" %}

{% if unity_articles.size > 0 or unity_posts.size > 0 %}
<ul>
  {% for page in unity_articles %}
  <li><a href="{{ page.url | relative_url }}">{{ page.title }}</a></li>
  {% endfor %}
  {% for post in unity_posts %}
  <li><a href="{{ post.url | relative_url }}">{{ post.title }}</a> — {{ post.date | date: "%Y-%m-%d" }}</li>
  {% endfor %}
</ul>
{% else %}
<p><em>아직 게시된 글이 없습니다.</em></p>
{% endif %}
